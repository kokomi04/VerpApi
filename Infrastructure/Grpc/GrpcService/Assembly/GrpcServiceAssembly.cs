using GrpcProto.Protos;
using GrpcService.Service;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;

namespace GrpcService.Assembly
{
    public static class GrpcServiceAssembly
    {
        public static System.Reflection.Assembly Assembly => typeof(GrpcServiceAssembly).Assembly;
    }
}
