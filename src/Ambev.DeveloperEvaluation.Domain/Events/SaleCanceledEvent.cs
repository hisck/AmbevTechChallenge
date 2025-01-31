﻿using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Common.Events;

namespace Ambev.DeveloperEvaluation.Domain.Events
{
    public class SaleCancelledEvent : DomainEvent
    {
        public Sale Sale { get; }

        public SaleCancelledEvent(Sale sale)
        {
            Sale = sale;
        }
    }
}
