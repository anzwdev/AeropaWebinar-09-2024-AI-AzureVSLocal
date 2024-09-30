namespace AnZw.AIDemo;
using System.AI;

codeunit 57202 AnZwAzureOpenAIDirectClient
{

    procedure SendMessage(MessageText: Text): Text
    var
        AnZwAzureAISetup: Record AnZwAzureAISetup;
        Client: HttpClient;
        Content: HttpContent;
        Response: HttpResponseMessage;
        ChatRequestJson: JsonObject;
        ChatRequestText: Text;
        ChatResponseText: Text;
        ApiKey: SecretText;
        Url: Text;
        ContentHeaders: HttpHeaders;
    begin
        ChatRequestJson := CreateOpenAIChatRequest(0.7, MessageText);
        ChatRequestJson.WriteTo(ChatRequestText);
        Content.WriteFrom(ChatRequestText);
        Content.GetHeaders(ContentHeaders);
        ContentHeaders.Clear();
        ContentHeaders.Add('ContentType', 'application/json');

        Client.Timeout := 120000;
        AnZwAzureAISetup.Get();
        IsolatedStorage.Get('AnZwAzureAIApiKey', ApiKey);
        Url := AnZwAzureAISetup.Endpoint;
        if (not (Url.EndsWith('/'))) then
            Url := Url + '/';
        Url := Url + 'openai/deployments/' + AnZwAzureAISetup.Deployment + '/chat/completions?api-version=2023-03-15-preview';
        Client.DefaultRequestHeaders.Add('api-key', ApiKey);
        Client.Post(Url, Content, Response);

        Response.Content.ReadAs(ChatResponseText);

        ChatResponseText := GetResponseContent(ChatResponseText);
        exit(ChatResponseText);
    end;

    local procedure GetResponseContent(Response: Text): Text
    var
        ResponseJson: JsonObject;
        ChoicesJsonToken: JsonToken;
        ChoicesJsonArray: JsonArray;
        SingleChoiceJsonToken: JsonToken;
        MessageJsonToken: JsonToken;
        ContentJsonToken: JsonToken;
        ContentText: Text;
    begin
        ResponseJson.ReadFrom(Response);
        ResponseJson.Get('choices', ChoicesJsonToken);
        ChoicesJsonArray := ChoicesJsonToken.AsArray();
        if (ChoicesJsonArray.Count > 0) then begin
            ChoicesJsonArray.Get(0, SingleChoiceJsonToken);
            SingleChoiceJsonToken.AsObject().Get('message', MessageJsonToken);
            MessageJsonToken.AsObject().Get('content', ContentJsonToken);
            ContentText := ContentJsonToken.AsValue().AsText();
        end;

        exit(ContentText);
    end;

    local procedure CreateOpenAIChatRequest(Temperature: Decimal; MessageText: Text): JsonObject
    var
        MessageJson: JsonObject;
    begin
        MessageJson.Add('messages', CreateOpenAIChatMessagesArray(MessageText));
        //MessageJson.Add('temperature', Temperature);
        exit(MessageJson);
    end;

    local procedure CreateOpenAIChatMessagesArray(MessageText: Text): JsonArray
    var
        MessagesArrayJson: JsonArray;
    begin
        MessagesArrayJson.Add(CreateOpenAIChatMessage('system', GetSystemPromptMessage()));
        MessagesArrayJson.Add(CreateOpenAIChatMessage('user', MessageText));
        exit(MessagesArrayJson);
    end;

    //role: system or user
    local procedure CreateOpenAIChatMessage(Role: Text; Content: Text): JsonObject
    var
        MessageJson: JsonObject;
    begin
        if (Role = '') then
            Role := 'user';
        MessageJson.Add('role', Role);
        MessageJson.Add('content', Content);
        exit(MessageJson);
    end;


    local procedure SetAuthorization(var AzureOpenAI: Codeunit "Azure OpenAI")
    var
        ApiKey: SecretText;
        AnZwAzureAISetup: Record AnZwAzureAISetup;
    begin
        AnZwAzureAISetup.Get();
        IsolatedStorage.Get('AnZwAzureAIApiKey', ApiKey);
        AzureOpenAI.SetAuthorization(Enum::"AOAI Model Type"::"Chat Completions", AnZwAzureAISetup.Endpoint, AnZwAzureAISetup.Deployment, ApiKey);
    end;

    local procedure SetParameters(var AOAIChatCompletionParams: Codeunit "AOAI Chat Completion Params")
    begin
        AOAIChatCompletionParams.SetMaxTokens(2500);
        AOAIChatCompletionParams.SetTemperature(0.7);
    end;

    local procedure GetSystemPromptMessage(): Text
    var
        newLine: Text;
    begin
        newLine := ' ';
        newLine[1] := 13;
        //newLine := '\n';

        exit(
            'Order data extractor in json format from text. ' +
            'You are a converter that reads text from a user ordering items, extract order information and returns it ' +
            'in the this json format without asking any additional questions or adding any text before or after json data:' + newLine +
            '{' + newLine +
            '  "customer": "customer_name",' + newLine +
            '  "items": [' + newLine +
            '    {' + newLine +
            '      "name": "item name",' + newLine +
            '      "quantity": "item quantity"' + newLine +
            '    }' + newLine +
            '  ]' + newLine +
            '}'
        );
    end;
}