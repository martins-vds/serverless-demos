$params = @{
    Uri = 'https://serveless-portal.azurewebsites.net/api/BasicFuntion'
    ContentType = 'application/json'
    Method = 'post'
    Body = '{
        "name": "Vinny"
    }'
}

Invoke-WebRequest @params