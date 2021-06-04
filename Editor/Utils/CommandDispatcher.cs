using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CZToolKit.GraphProcessor.Editors
{
    public class CommandDispatcher
    {
        public delegate void CommandHandler(Command _command);


        Dictionary<Type, CommandHandler> commandHandlers = new Dictionary<Type, CommandHandler>();

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
    }
}
