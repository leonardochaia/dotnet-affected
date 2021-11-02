#!/bin/bash
set -e

docker build -t lchaia/dotnet-affected:latest - < Dockerfile
