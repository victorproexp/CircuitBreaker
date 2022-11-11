#!/bin/bash

for i in {1..1000}
do
curl -sk  http://localhost:4000/weatherforecast/GetServiceA
echo -e '\n'
done