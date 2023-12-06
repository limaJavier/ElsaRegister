using Elsa.Activities.Email;
using Elsa.Activities.Primitives;
using Elsa.Builders;

namespace ElsaRegister.Workflows;

public class RegisterAdministrationWorkflow : IWorkflow
{
    public void Build(IWorkflowBuilder builder)
    {
        builder
            .SetVariable("user", context => context.Input)
            .SendEmail(activity => activity
                .WithSender("workflow@acme.com")
                .WithRecipient("admin1@acme.com")
                .WithSubject(context => $"{context.GetVariable<dynamic>("user")!.Name} is waiting for your review!")
                .WithBody(context =>
                        {
                            var name = context.GetVariable<dynamic>("user")!.Name;
                            var email = context.GetVariable<dynamic>("user")!.Email;
                            return $"Don't forget to review user request {name}.<br><a href=\"http://localhost:5043/register/{email}1\">Approve</a> or <a href=\"http://localhost:5043/register/{email}0\">Reject</a>";
                        }))
            .SendEmail(activity => activity
                .WithSender("workflow@acme.com")
                .WithRecipient("admin2@acme.com")
                .WithSubject(context => $"{context.GetVariable<dynamic>("user")!.Name} is waiting for your review!")
                .WithBody(context =>
                        {
                            var name = context.GetVariable<dynamic>("user")!.Name;
                            var email = context.GetVariable<dynamic>("user")!.Email;
                            return $"Don't forget to review user request {name}.<br><a href=\"http://localhost:5043/register/{email}1\">Approve</a> or <a href=\"http://localhost:5043/register/{email}0\">Reject</a>";
                        }));
    }
}
