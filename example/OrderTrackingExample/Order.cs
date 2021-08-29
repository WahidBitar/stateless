using System;
using System.Collections.Generic;
using System.Linq;
using Stateless;

namespace OrderTrackingExample
{
    class Order
    {
        private readonly StateMachine<OrderState.States, BaseTrigger.Triggers> _machine;
        private readonly StateMachine<OrderState.States, BaseTrigger.Triggers>.TriggerWithParameters<CompleteTrigger> completeTrigger;
        private readonly StateMachine<OrderState.States, BaseTrigger.Triggers>.TriggerWithParameters<CancelTrigger> cancelTrigger;
        private readonly StateMachine<OrderState.States, BaseTrigger.Triggers>.TriggerWithParameters<RecordNoteTrigger> recordNoteTrigger;
        private IList<OrderState> _states;


        public Order(string number)
        {
            Number = number;
            _states = new List<OrderState>();


            CurrentState = new OrderState(OrderState.States.Opened, DateTimeOffset.UtcNow);

            _machine = new StateMachine<OrderState.States, BaseTrigger.Triggers>(() => CurrentState.State, s => CurrentState = new OrderState(s, DateTimeOffset.UtcNow));

            recordNoteTrigger = _machine.SetTriggerParameters<RecordNoteTrigger>(BaseTrigger.Triggers.RecordNote);
            completeTrigger = _machine.SetTriggerParameters<CompleteTrigger>(BaseTrigger.Triggers.Complete);
            cancelTrigger = _machine.SetTriggerParameters<CancelTrigger>(BaseTrigger.Triggers.Cancel);

            _machine.Configure(OrderState.States.Opened)
                .PermitReentryIf(BaseTrigger.Triggers.Create, () => _states.Count == 0)
                .InternalTransitionIf(recordNoteTrigger, trigger => !string.IsNullOrWhiteSpace(trigger.Note), (trigger, transition) => { })
                .Permit(BaseTrigger.Triggers.Accept, OrderState.States.Inprogress);

            _machine.Configure(OrderState.States.Inprogress)
                .SubstateOf(OrderState.States.Opened)
                .PermitIf(completeTrigger, OrderState.States.Completed, trigger => !string.IsNullOrWhiteSpace(trigger.CompleteNotes))
                .PermitIf(cancelTrigger, OrderState.States.Canceled, trigger => !string.IsNullOrWhiteSpace(trigger.User));

            _machine.Configure(OrderState.States.Completed)
                .PermitReentryIf(recordNoteTrigger, trigger => !string.IsNullOrWhiteSpace(trigger.Note));

            _machine.Configure(OrderState.States.Canceled)
                .SubstateOf(OrderState.States.Completed);

            _machine.OnTransitionCompleted(onStateChanged);

            _machine.Fire(BaseTrigger.Triggers.Create);
        }

        private void onStateChanged(StateMachine<OrderState.States, BaseTrigger.Triggers>.Transition transition)
        {
            var previousState = _states.OrderByDescending(s => s.Start).FirstOrDefault();

            if (previousState is not null)
                previousState.End = DateTimeOffset.UtcNow;

            _states.Add(CurrentState);
        }

        public string Number { get; set; }
        public OrderState CurrentState { get; private set; }
        public OrderState[] States => _states.ToArray();

        public void Accept()
        {
            _machine.Fire(BaseTrigger.Triggers.Accept);
        }

        public void RecordNote(string note)
        {
            _machine.Fire(recordNoteTrigger, new RecordNoteTrigger() { Note = note, TriggerDate = DateTimeOffset.UtcNow });
        }

        public void Complete(string note)
        {
            _machine.Fire(completeTrigger, new CompleteTrigger() { CompleteNotes = note, TriggerDate = DateTimeOffset.UtcNow });
        }

        public void Cancel(string user)
        {
            _machine.Fire(completeTrigger, new CancelTrigger() { User = user, TriggerDate = DateTimeOffset.UtcNow });
        }
    }
}