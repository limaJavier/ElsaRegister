using System.Net;
using System.Net.Http;
using AutoMapper;
using Elsa.Activities.Console;
using Elsa.Activities.ControlFlow;
using Elsa.Activities.Email;
using Elsa.Activities.Http;
using Elsa.Activities.Http.Extensions;
using Elsa.Activities.Http.Models;
using Elsa.Activities.Primitives;
using Elsa.Activities.Temporal;
using Elsa.Builders;
using Elsa.Models;
using ElsaRegister.Models;
using ElsaRegister.Services;
using FluentValidation;
using NodaTime;

namespace ElsaGuides.ContentApproval.Web
{
    public class RegisterWorkflow : IWorkflow
    {
        public void Build(IWorkflowBuilder builder)
        {
            builder
                .SetVariable("user", flow => flow.Input)
                .WriteLine("Workflow started")
                .Then<Fork>(activity => activity.WithBranches("Approve", "Reject", "Remind"), fork =>
                {
                    fork
                        .When("Approve")
                        .SignalReceived("Approve")
                        .SendEmail(activity => activity
                            .WithSender("workflow@acme.com")
                            .WithRecipient(context => context.GetVariable<dynamic>("user")!.Email)
                            .WithSubject("Request accepted")
                            .WithBody(context =>
                            {
                                var userName = context.GetVariable<dynamic>("user")!.Name;
                                return $"{userName} you have been accepted. Congratulations!";
                            }))
                        .ThenNamed("Join");

                    fork
                        .When("Reject")
                        .SignalReceived("Reject")
                        .SendEmail(activity => activity
                            .WithSender("workflow@acme.com")
                            .WithRecipient(context => context.GetVariable<dynamic>("user")!.Email)
                            .WithSubject("Request rejected")
                            .WithBody(context =>
                            {
                                var userName = context.GetVariable<dynamic>("user")!.Name;
                                return $"We're sorry, {userName}, you have been rejected.";
                            }))
                        .ThenNamed("Join");

                    fork
                        .When("Remind")
                        // .Timer(Duration.FromSeconds(10)).WithName("Reminder")
                        .SendEmail(activity => activity
                                .WithSender("workflow@acme.com")
                                .WithRecipient("admin1@acme.com")
                                .WithSubject(context => $"{context.GetVariable<dynamic>("user")!.Name} is waiting for your review!")
                                .WithBody(context =>
                                    $"Don't forget to review user request {context.GetVariable<dynamic>("user")!.Email}.<br><a href=\"{context.GenerateSignalUrl("Approve")}\">Approve</a> or <a href=\"{context.GenerateSignalUrl("Reject")}\">Reject</a>"))
                        // .ThenNamed("Reminder");
                        .Break();
                })
                .Add<Join>(join => join.WithMode(Join.JoinMode.WaitAny)).WithName("Join");
        }
    }
}