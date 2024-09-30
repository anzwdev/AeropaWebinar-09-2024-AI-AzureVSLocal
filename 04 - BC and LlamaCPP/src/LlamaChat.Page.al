namespace AnZw.AIDemo;

page 57100 LlamaChat
{
    PageType = PromptDialog;
    Extensible = false;
    ApplicationArea = All;
    UsageCategory = Administration;

    layout
    {
        area(Prompt)
        {
            field(UserChatMessage; InputMessageText)
            {
                ApplicationArea = All;
                ShowCaption = false;
                MultiLine = true;
                InstructionalText = 'Write message that you want to send to your AI model.';
            }
        }
        area(Content)
        {
            field("Response"; ResponseMessageText)
            {
                ApplicationArea = All;
                MultiLine = true;
                Caption = 'AI Model Response';
            }
        }
    }

    actions
    {
        area(SystemActions)
        {
            systemaction(Generate)
            {
                Caption = 'Send';
                ToolTip = 'Send message to the AI model';

                trigger OnAction()
                begin
                    SendMessage();
                end;
            }
        }
    }


    var
        InputMessageText: Text;
        ResponseMessageText: Text;

    procedure SendMessage()
    var
        AIClient: Codeunit LlamaCppClient;
    begin
        ResponseMessageText := AIClient.SendMessage(InputMessageText);
        InputMessageText := '';
        CurrPage.Update(false);
    end;

}