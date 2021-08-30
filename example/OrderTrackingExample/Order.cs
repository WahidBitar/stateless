using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Stateless;

namespace OrderTrackingExample
{
    class Order
    {
        private readonly StateMachine<OrderState.States, BaseTrigger.Triggers> _machine;
        private readonly StateMachine<OrderState.States, BaseTrigger.Triggers>.TriggerWithParameters<CompleteTrigger> completeTrigger;
        private readonly StateMachine<OrderState.States, BaseTrigger.Triggers>.TriggerWithParameters<CancelTrigger> cancelTrigger;
        private readonly StateMachine<OrderState.States, BaseTrigger.Triggers>.TriggerWithParameters<RecordNoteTrigger> recordNoteTrigger;
        private readonly StateMachine<OrderState.States, BaseTrigger.Triggers>.TriggerWithParameters<UpdateWorkDataTrigger> updateWorkDataTrigger;
        private HashSet<OrderState> _states;


        public Order(string number)
        {
            Number = number;
            _states = new HashSet<OrderState>();

            _machine = new StateMachine<OrderState.States, BaseTrigger.Triggers>(() => CurrentState?.State ?? OrderState.States.Created, s =>
                {
                    CurrentState = OrderState.Create(s);
                });

            recordNoteTrigger = _machine.SetTriggerParameters<RecordNoteTrigger>(BaseTrigger.Triggers.RecordNote);
            updateWorkDataTrigger = _machine.SetTriggerParameters<UpdateWorkDataTrigger>(BaseTrigger.Triggers.UpdateWorkData);
            completeTrigger = _machine.SetTriggerParameters<CompleteTrigger>(BaseTrigger.Triggers.Complete);
            cancelTrigger = _machine.SetTriggerParameters<CancelTrigger>(BaseTrigger.Triggers.Cancel);

            _machine.Configure(OrderState.States.Created)
                .Permit(BaseTrigger.Triggers.Create, OrderState.States.Opened);

            _machine.Configure(OrderState.States.Opened)
                .InternalTransitionIf(recordNoteTrigger, trigger => !string.IsNullOrWhiteSpace(trigger.Note), onInternalTransition)
                .Permit(BaseTrigger.Triggers.Accept, OrderState.States.Inprogress);

            _machine.Configure(OrderState.States.Inprogress)
                .SubstateOf(OrderState.States.Opened)
                .PermitReentry(BaseTrigger.Triggers.UpdateWork)
                .PermitReentryIf(updateWorkDataTrigger, trigger => !string.IsNullOrWhiteSpace(trigger.Data))
                .PermitIf(completeTrigger, OrderState.States.Completed, trigger => !string.IsNullOrWhiteSpace(trigger.CompleteNotes))
                .PermitIf(cancelTrigger, OrderState.States.Canceled, trigger => !string.IsNullOrWhiteSpace(trigger.User));

            _machine.Configure(OrderState.States.Completed)
                .InternalTransitionIf(recordNoteTrigger, trigger => !string.IsNullOrWhiteSpace(trigger.Note), onInternalTransition);

            _machine.Configure(OrderState.States.Canceled)
                .SubstateOf(OrderState.States.Completed);

            _machine.OnTransitionCompleted(onStateChanged);

            _machine.Fire(BaseTrigger.Triggers.Create);
        }


        private void onInternalTransition<TArg0>(TArg0 arg, StateMachine<OrderState.States, BaseTrigger.Triggers>.Transition transition)
        {
            switch (arg)
            {
                case RecordNoteTrigger noteTrigger:
                    CurrentState.Notes.Add(noteTrigger.Note);
                    break;
            }
        }


        private void onStateChanged(StateMachine<OrderState.States, BaseTrigger.Triggers>.Transition transition)
        {
            if (CurrentState is null)
                return;

            var lastState = _states.OrderByDescending(s => s.Start).FirstOrDefault();

            if (lastState != null && transition.IsReentry && lastState.State == transition.Destination)
            {
                lastState.Triggers.Add(new TriggerData
                {
                    Trigger = transition.Trigger,
                    TriggerDate = DateTimeOffset.UtcNow,
                });
            }
            else
            {
                if (lastState != null)
                    lastState.End = DateTimeOffset.UtcNow;

                CurrentState.Triggers.Add(new TriggerData
                {
                    Trigger = transition.Trigger,
                    TriggerDate = DateTimeOffset.UtcNow,
                });
                _states.Add(CurrentState);
            }
        }


        public string Number { get; set; }
        public OrderState CurrentState { get; private set; }
        public OrderState[] States => _states.ToArray();

        public void Accept()
        {
            _machine.Fire(BaseTrigger.Triggers.Accept);
        }
        public void Update()
        {
            _machine.Fire(BaseTrigger.Triggers.UpdateWork);
        }


        public void UpdateData(string data)
        {
            _machine.Fire(updateWorkDataTrigger, new UpdateWorkDataTrigger { Data = data, TriggerDate = DateTimeOffset.UtcNow });
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
            _machine.Fire(cancelTrigger, new CancelTrigger() { User = user, TriggerDate = DateTimeOffset.UtcNow });
        }
    }
}