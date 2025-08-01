/* -------------------------------------------------------------------------------------------------
   Copyright (C) Siemens Healthcare GmbH 2025, All rights reserved. Restricted.
   ------------------------------------------------------------------------------------------------- */
#nullable enable
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace cart.services
{
    internal class CartServiceEventLogger
    {
        private readonly ILogger _logger;

        internal CartServiceEventLogger(ILogger logger)
        {
            _logger = logger;
        }

        internal void AddItem(string userId, string productId, int quantity, bool isError, string failureReason)
        {
            EventId eventId = new EventId(102, "Cart.AddItem");
            List<KeyValuePair<string, object?>> attributes = new List<KeyValuePair<string, object?>>()
               {
                   new("sy.eventname", "Cart.AddItem"),
                   new("cart.userId", userId),
                   new("cart.productId", productId),
                   new("cart.quantity", quantity)
               };
            if (isError)
            {
                attributes.Add(new KeyValuePair<string, object?>("sy.status", "failure"));
                attributes.Add(new KeyValuePair<string, object?>("sy.failureReason", failureReason));
            }
            else
            {
                attributes.Add(new KeyValuePair<string, object?>("sy.status", "success"));
            }
            _logger.Log(LogLevel.Information, eventId, attributes, null, (state, exception) => "Event Cart.AddItem");
        }

        internal void EmptyCart(string userId, bool isError, string failureReason)
        {
            EventId eventId = new EventId(101, "Cart.EmptyCart");
            List<KeyValuePair<string, object?>> attributes = new List<KeyValuePair<string, object?>>()
               {
                   new("sy.eventname", "Cart.EmptyCart"),
                   new("cart.userId", userId)
               };
            if (isError)
            {
                attributes.Add(new KeyValuePair<string, object?>("sy.status", "failure"));
                attributes.Add(new KeyValuePair<string, object?>("sy.failureReason", failureReason));
            }

            _logger.Log(LogLevel.Information, eventId, attributes, null, (state, exception) => "Event Cart.EmptyCart");
        }
    }
}
