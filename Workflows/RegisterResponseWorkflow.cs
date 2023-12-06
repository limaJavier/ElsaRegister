using Elsa.Activities.Email;
using Elsa.Activities.Primitives;
using Elsa.Builders;


namespace ElsaRegister.Workflows;
public class RegisterResponseWorkflow : IWorkflow
{
    public void Build(IWorkflowBuilder builder)
    {
        builder
            .SetVariable("data", context => context.Input)
            .SendEmail(activity => activity
                        .WithSender("workflow@acme.com")
                        .WithRecipient(context => context.GetVariable<dynamic>("data")!.Email)
                        .WithSubject(context => "Request response")
                        .WithBody(context =>
                        {
                            var email = (string)context.GetVariable<dynamic>("data")!.Email;
                            var status = (string)context.GetVariable<dynamic>("data")!.Encoding;

                            if(status == "accepted")
                                return "Congratulations! You have been accepted!";
                            else if(status == "wait")
                                return "Half way there! Please, wait for last confirmation";
                            else
                                return "Sorry! You have been rejected.";
                        }));
    }
}