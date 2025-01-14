using System;
using System.Collections.Generic;

namespace ClassRichPresence.State
{
    public static class AppStateManager
    {
        private static readonly Dictionary<Type, IAppState> _states = new Dictionary<Type, IAppState>();

        public static IAppState CurrentState { get; set; }

        static AppStateManager()
        {
            _states.Add(typeof(MenuState), new MenuState());
            _states.Add(typeof(StudyState), new StudyState());
            _states.Add(typeof(SubjectAddState), new SubjectAddState());
            _states.Add(typeof(SubjectDeleteState), new SubjectDeleteState());
        }

        public static IAppState GetState<T>() where T : IAppState
        {
            return GetState(typeof(T));
        }

        public static IAppState GetState(Type stateType)
        {
            return _states[stateType];
        }
    }
}
