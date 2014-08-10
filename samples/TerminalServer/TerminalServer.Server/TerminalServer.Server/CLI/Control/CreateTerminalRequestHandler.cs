﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerminalServer.Server.Infrastructure;
using TerminalServer.Server.Messaging;
using TerminalServer.Server.Messaging.TerminalControl;

namespace TerminalServer.Server.CLI.Control
{
    public class CreateTerminalRequestHandler : IObserver<RequestBase>, IObservable<EventBase>
    {
        readonly CliSessionAbstractFactory _factory;
        readonly CliControl _control;
        readonly List<IObserver<EventBase>> _subscriptions;
        readonly ILogger _log;

        public CreateTerminalRequestHandler(CliControl control, CliSessionAbstractFactory factory, ILogger log)
        {
            _factory = factory;
            _control = control;
            _log = log;
            _subscriptions = new List<IObserver<EventBase>>();
        }
        public IDisposable Subscribe(IObserver<EventBase> observer)
        {
            _subscriptions.Add(observer);
            return new Subscription(() => _subscriptions.Remove(observer));
        }
        public void OnCompleted()
        {
        }
        public void OnError(Exception error)
        {
        }
        public void OnNext(RequestBase req)
        {
            if (TerminalControlRequest.Label != req.Label || req.Command != CreateTerminalRequest.Command)
                return;

            var cte = (CreateTerminalRequest)req;
            var cli = _factory.Create(cte.Type);
            _control.AddSession(cli);
            foreach (var subscription in _subscriptions)
                subscription.OnNext(new CreatedTerminalEvent(cli.Id, cli.Type, cte.CorrelationId));
        }
        ~CreateTerminalRequestHandler()
        {
            _log.Debug(this.GetType().Name + " destroy");
        }
    }
}
