using System;
using System.Collections.Generic;

namespace OrderTrackingExample
{
    abstract class OrderState : IEquatable<OrderState>
    {
        internal enum States
        {
            Opened,
            Inprogress,
            Canceled,
            Completed
        }

        public OrderState(States state, DateTimeOffset start)
        {
            State = state;
            Start = start;
            Notes = new List<string>();
        }

        internal States State { get; set; }
        internal DateTimeOffset Start { get; private set; }
        internal DateTimeOffset? End { get; set; }
        internal ICollection<string> Notes { get; private set; }

        public bool Equals(OrderState other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return State == other.State && Start.Equals(other.Start);
        }

        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) || obj is OrderState other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine((int)State, Start);
        }

        public static bool operator ==(OrderState left, OrderState right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(OrderState left, OrderState right)
        {
            return !Equals(left, right);
        }

        public static OrderState Create(States state)
        {
            switch (state)
            {
                case States.Opened:
                    return new OpenedState();
                case States.Inprogress:
                    return new InprogressState();
                case States.Canceled:
                    return new CanceledState();
                case States.Completed:
                    return new CompletedState();
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }
    }

    class OpenedState : OrderState
    {
        public OpenedState() : base(States.Opened, DateTimeOffset.UtcNow)
        {
        }
    }

    class InprogressState : OrderState
    {
        public InprogressState() : base(States.Inprogress, DateTimeOffset.UtcNow)
        {
        }
    }

    class CanceledState : OrderState
    {
        public CanceledState() : base(States.Canceled, DateTimeOffset.UtcNow)
        {
        }
    }

    class CompletedState : OrderState
    {
        public CompletedState() : base(States.Completed, DateTimeOffset.UtcNow)
        {
        }
    }
}