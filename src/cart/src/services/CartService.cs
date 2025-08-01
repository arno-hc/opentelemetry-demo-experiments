// Copyright The OpenTelemetry Authors
// SPDX-License-Identifier: Apache-2.0
using System.Diagnostics;
using System.Threading.Tasks;
using System;
using Grpc.Core;
using cart.cartstore;
using OpenFeature;
using Oteldemo;
using Microsoft.Extensions.Logging;

namespace cart.services;

public class CartService : Oteldemo.CartService.CartServiceBase
{
    private static readonly Empty Empty = new();
    private readonly Random random = new Random();
    private readonly ICartStore _badCartStore;
    private readonly ICartStore _cartStore;
    private readonly IFeatureClient _featureFlagHelper;
    private readonly ILogger _logger;
    private readonly CartServiceEventLogger _eventLogger;

    public CartService(ICartStore cartStore, ICartStore badCartStore, IFeatureClient featureFlagService, ILogger<CartService> logger)
    {
        _badCartStore = badCartStore;
        _cartStore = cartStore;
        _featureFlagHelper = featureFlagService;
        _logger = logger;
        _eventLogger = new CartServiceEventLogger(logger);
    }

    public override async Task<Empty> AddItem(AddItemRequest request, ServerCallContext context)
    {
        var activity = Activity.Current;
        activity?.SetTag("app.user.id", request.UserId);
        activity?.SetTag("app.product.id", request.Item.ProductId);
        activity?.SetTag("app.product.quantity", request.Item.Quantity);
        string failureReason = string.Empty;
        try
        {
            await _cartStore.AddItemAsync(request.UserId, request.Item.ProductId, request.Item.Quantity);

            return Empty;
        }
        catch (RpcException ex)
        {
            failureReason = ex.Message;
            activity?.AddException(ex);
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            throw;
        }
        finally
        {
            _eventLogger.AddItem(request.UserId, request.Item.ProductId, request.Item.Quantity, Activity.Current?.Status == ActivityStatusCode.Error, failureReason);
        }
    }

    public override async Task<Cart> GetCart(GetCartRequest request, ServerCallContext context)
    {
        var activity = Activity.Current;
        activity?.SetTag("app.user.id", request.UserId);
        activity?.AddEvent(new("Fetch cart"));

        try
        {
            var cart = await _cartStore.GetCartAsync(request.UserId);
            var totalCart = 0;
            foreach (var item in cart.Items)
            {
                totalCart += item.Quantity;
            }
            activity?.SetTag("app.cart.items.count", totalCart);

            return cart;
        }
        catch (RpcException ex)
        {
            activity?.AddException(ex);
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            throw;
        }
    }

    public override async Task<Empty> EmptyCart(EmptyCartRequest request, ServerCallContext context)
    {
        var activity = Activity.Current;
        activity?.SetTag("app.user.id", request.UserId);
        activity?.AddEvent(new("Empty cart"));
        bool isError = false;
        string failureReason = string.Empty;
        try
        {
            if (await _featureFlagHelper.GetBooleanValueAsync("cartFailure", false))
            {
                await _badCartStore.EmptyCartAsync(request.UserId);
            }
            else
            {
                await _cartStore.EmptyCartAsync(request.UserId);
            }
        }
        catch (RpcException ex)
        {
            failureReason = ex.Message;
            isError = true;
            Activity.Current?.AddException(ex);
            Activity.Current?.SetStatus(ActivityStatusCode.Error, ex.Message);
            throw;
        }
        finally
        {
            _eventLogger.EmptyCart(request.UserId, isError, failureReason);
        }

        return Empty;
    }
}
