using System;

namespace OrderTrackingExample
{
    abstract class BaseTrigger : IEquatable<BaseTrigger>
    {
        internal enum Triggers
        {
            RecordNote,
            Accept,
            Cancel,
            Complete,
            Create
        }

        internal abstract Triggers Trigger { get; }
        internal DateTimeOffset TriggerDate { get; init; }

        public bool Equals(BaseTrigger other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Trigger == other.Trigger && TriggerDate.Equals(other.TriggerDate);
        }

        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) || obj is BaseTrigger other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine((int)Trigger, TriggerDate);
        }

        public static bool operator ==(BaseTrigger left, BaseTrigger right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(BaseTrigger left, BaseTrigger right)
        {
            return !Equals(left, right);
        }
    }

    class RecordNoteTrigger : BaseTrigger
    {
        internal override Triggers Trigger => Triggers.RecordNote;
        public string Note { get; init; }
    }

    class AcceptTrigger : BaseTrigger
    {
        internal override Triggers Trigger => Triggers.Accept;
    }

    class CancelTrigger : BaseTrigger
    {
        internal override Triggers Trigger => Triggers.Cancel;
        public string User { get; init; }
    }

    class CompleteTrigger : BaseTrigger
    {
        internal override Triggers Trigger => Triggers.Complete;
        internal string CompleteNotes { get; init; }
    }
}