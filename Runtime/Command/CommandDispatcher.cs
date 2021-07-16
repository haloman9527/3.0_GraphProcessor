#region 注 释
/***
 *
 *  Title:
 *  
 *  Description:
 *  
 *  Date:
 *  Version:
 *  Writer: 半只龙虾人
 *  Github: https://github.com/HalfLobsterMan
 *  Blog: https://www.crosshair.top/
 *
 */
#endregion
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CZToolKit.GraphProcessor
{
    public class CommandDispatcher
    {
        public delegate void CommandHandler(Command _command);

        readonly object syncRoot = new object();

        readonly Dictionary<Type, CommandHandler> commandHandlers = new Dictionary<Type, CommandHandler>();

        readonly Dictionary<string, List<IStateObserver>> stateObservers = new Dictionary<string, List<IStateObserver>>();
        readonly HashSet<IStateObserver> observerCallSet = new HashSet<IStateObserver>();

        public GraphState GraphState { get; }

        public CommandDispatcher(GraphState _graphState)
        {
            GraphState = _graphState;
        }

        /// <summary> 为命令类型注册一个命令处理器 </summary>
        /// <typeparam name="TCommand"> 命令类型 </typeparam>
        /// <param name="_commandHandler"> 命令处理器 </param>
        public void RegisterCommandHandler<TCommand>(CommandHandler _commandHandler) where TCommand : Command
        {
            commandHandlers[typeof(TCommand)] = _commandHandler;
        }

        /// <summary> 注销指定命令类型的命令处理器 </summary>
        /// <typeparam name="TCommand"> 命令类型 </typeparam>
        public void UnregisterCommandHandler<TCommand>() where TCommand : Command
        {
            commandHandlers.Remove(typeof(TCommand));
        }

        public void Dispatch(Command _command)
        {
            if (_command == null) return;

            Type commandType = _command.GetType();

            if (!commandHandlers.TryGetValue(commandType, out CommandHandler commandHandler))
                return;

            commandHandler.Invoke(_command);
        }

        public void RegisterCommandObserver(Action<Command> observer)
        {
            lock (syncRoot)
            {
                //if (m_CommandObservers.Contains(observer))
                //    throw new InvalidOperationException("Cannot register the same observer twice.");
                //m_CommandObservers.Add(observer);
            }
        }

        public void UnregisterCommandObserver(Action<Command> observer)
        {
            lock (syncRoot)
            {
                //if (m_CommandObservers.Contains(observer))
                //{
                //    m_CommandObservers.Remove(observer);
                //}
            }
        }

        public void Registerobserver(IStateObserver _observer)
        {
            if (_observer == null)
                return;

            lock (syncRoot)
            {
                //foreach (var component in _observer.observedstatecomponents)
                //{
                //    if (!stateObservers.TryGetValue(component, out var observerforcomponent))
                //        stateObservers[component] = observerforcomponent = new List<IStateObserver>();

                //    if (!observerforcomponent.Contains(_observer))
                //        observerforcomponent.Add(_observer);
                //}
            }
        }

        //public void UnregisterObserver(IStateObserver observer)
        //{
        //    if (observer == null)
        //        return;

        //    lock (syncRoot)
        //    {
        //        // We do it this way in case observer.ObservedStateComponents changed since RegisterObserver() was called.
        //        foreach (var observersByComponent in m_StateObservers)
        //        {
        //            observersByComponent.Value.Remove(observer);
        //        }
        //    }
        //}

        public void NotifyObservers()
        {

        }
    }
}