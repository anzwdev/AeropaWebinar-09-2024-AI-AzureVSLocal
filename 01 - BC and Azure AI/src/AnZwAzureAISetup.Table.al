table 57200 AnZwAzureAISetup
{
    DataClassification = SystemMetadata;

    fields
    {
        field(1; PKey; Code[10])
        {
            Caption = 'PKey';
        }
        field(2; "Endpoint"; Text[100])
        {
            Caption = 'Endpoint';
            ToolTip = 'Url to your Azure OpenAI service.';
        }
        field(3; "Deployment"; Text[100])
        {
            Caption = 'Deployment';
            ToolTip = 'Name of your model.';
        }
    }

    keys
    {
        key(Key1; PKey)
        {
            Clustered = true;
        }
    }

}