#!/bin/sh
# dotnet tool install --global dotnet-sql-cache

run_cmd="dotnet run --server.urls http://*:5000"

until /root/.dotnet/tools/dotnet-ef database update; do
>&2 echo "SQL Server is starting up"
sleep 1
done

/root/.dotnet/tools/dotnet-sql-cache create "Data Source=db;Initial Catalog=DistCache;User Id=sa; Password=$SA_PASSWORD;" dbo TestCache
# /root/.dotnet/tools/dotnet-ef database update

exec $run_cmd

