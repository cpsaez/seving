How to run rabbit on docker
---------------------------
docker run -d --restart always --hostname hostrabbitmq --name rabbitmq1 -p 15672:15672 -p 5672:5672  -e RABBITMQ_DEFAULT_USER=lendsum -e RABBITMQ_DEFAULT_PASS=Lendsum12345 rabbitmq:3-management 



Hot wo run sql server on docker
-----------------------------
docker run -d --restart always --name sqlserver1 -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=Lendsum12345" -p 1433:1433 -d mcr.microsoft.com/mssql/server:2019-latest

**after this command, you have to connect to sql server in your localhost and create the database "sevingIntegratedTests"