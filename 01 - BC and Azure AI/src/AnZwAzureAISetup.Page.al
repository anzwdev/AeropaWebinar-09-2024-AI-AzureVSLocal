page 57201 AnZwAzureAISetup
{
    PageType = Card;
    ApplicationArea = All;
    UsageCategory = Administration;
    SourceTable = AnZwAzureAISetup;

    layout
    {
        area(Content)
        {
            group(GroupName)
            {
                Caption = 'General';

                field(Endpoint; Rec.Endpoint)
                {
                }
                field(Deployment; Rec.Deployment)
                {
                }
                field(ApiKey; ApiKey)
                {
                    ExtendedDatatype = Masked;

                    trigger OnValidate()
                    var
                        SecretApiKey: SecretText;
                    begin
                        if (ApiKey <> '') then begin
                            SecretApiKey := ApiKey;
                            IsolatedStorage.Set('AnZwAzureAIApiKey', SecretApiKey);
                            ApiKey := '';
                            CurrPage.Update(false);
                            Message('API Key Updated.');
                        end else
                            Message('API Key was empty and was not saved.');
                    end;

                }
            }
        }
    }

    var
        ApiKey: Text;

    trigger OnOpenPage()
    begin
        if (not (Rec.Get)) then begin
            Rec.Init();
            Rec.Insert();
        end;
    end;

}