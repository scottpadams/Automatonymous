// Copyright 2011-2015 Chris Patterson, Dru Sellers
// 
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Automatonymous.Binders
{
    using System;
    using System.Collections.Generic;


    public class TriggerEventActivityBinder<TInstance> :
        EventActivityBinder<TInstance>
        where TInstance : class
    {
        readonly StateActivityBinder<TInstance>[] _activities;
        readonly Event _event;
        readonly StateMachine<TInstance> _machine;

        public TriggerEventActivityBinder(StateMachine<TInstance> machine, Event @event, params StateActivityBinder<TInstance>[] activities)
        {
            _event = @event;
            _machine = machine;
            _activities = activities ?? new StateActivityBinder<TInstance>[0];
        }

        TriggerEventActivityBinder(StateMachine<TInstance> machine, Event @event, StateActivityBinder<TInstance>[] activities,
            params StateActivityBinder<TInstance>[] appendActivity)
        {
            _event = @event;
            _machine = machine;

            _activities = new StateActivityBinder<TInstance>[activities.Length + appendActivity.Length];
            Array.Copy(activities, 0, _activities, 0, activities.Length);
            Array.Copy(appendActivity, 0, _activities, activities.Length, appendActivity.Length);
        }

        Event EventActivityBinder<TInstance>.Event
        {
            get { return _event; }
        }

        EventActivityBinder<TInstance> EventActivityBinder<TInstance>.Add(Activity<TInstance> activity)
        {
            StateActivityBinder<TInstance> activityBinder = new EventStateActivityBinder<TInstance>(_event, activity);

            return new TriggerEventActivityBinder<TInstance>(_machine, _event, _activities, activityBinder);
        }

        EventActivityBinder<TInstance> EventActivityBinder<TInstance>.Catch<T>(
            Func<CompensateActivityBinder<TInstance, T>, CompensateActivityBinder<TInstance, T>> activityCallback)
        {
            CompensateActivityBinder<TInstance, T> binder = new ExceptionCompensationActivityBinder<TInstance, T>();

            binder = activityCallback(binder);

            StateActivityBinder<TInstance> activityBinder = new CompensationStateActivityBinder<TInstance>(_event, binder);

            return new TriggerEventActivityBinder<TInstance>(_machine, _event, _activities, activityBinder);
        }

        StateMachine<TInstance> EventActivityBinder<TInstance>.StateMachine
        {
            get { return _machine; }
        }

        public IEnumerable<StateActivityBinder<TInstance>> GetStateActivityBinders()
        {
            return _activities;
        }
    }


    public class ExceptionCompensationActivityBinder<TInstance, TException> :
        CompensateActivityBinder<TInstance,TException>
        where TInstance : class
        where TException : Exception
    {
        public IEnumerable<StateActivityBinder<TInstance>> GetStateActivityBinders()
        {
            throw new NotImplementedException();
        }

        public StateMachine<TInstance> StateMachine
        {
            get { throw new NotImplementedException(); }
        }

        public Event Event
        {
            get { throw new NotImplementedException(); }
        }

        public CompensateActivityBinder<TInstance, TException> Add(Activity<TInstance> activity)
        {
            throw new NotImplementedException();
        }

        public CompensateActivityBinder<TInstance, T> Catch<T>(Func<CompensateActivityBinder<TInstance, T>, CompensateActivityBinder<TInstance, T>> activityCallback) where T : Exception
        {
            CompensateActivityBinder<TInstance, T> binder = new ExceptionCompensationActivityBinder<TInstance, T>();

            binder = activityCallback(binder);

            StateActivityBinder<TInstance> activityBinder = new CompensationStateActivityBinder<TInstance>(_event, binder);

            return new TriggerEventActivityBinder<TInstance>(_machine, _event, _activities, activityBinder);
        }
    }
}