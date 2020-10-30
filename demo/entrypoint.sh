#!/bin/sh
# dotnet tool install --global dotnet-sql-cache

sleep 5;
/root/.dotnet/tools/dotnet-sql-cache create "Data Source=db;Initial Catalog=DistCache;User Id=sa; Password=$SA_PASSWORD;" dbo TestCache

dotnet demo.dll

