$params = @{
    Uri = 'https://ey2.azurewebsites.net/api/HttpTrigger1'
    ContentType = 'application/json'
    Method = 'post'
    Body = '{
        "name": "John"
    }'
}

Invoke-WebRequest @params