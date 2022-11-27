﻿#pragma warning disable IDE0130 // Namespace does not match the folder structure

namespace EndpointMapper;

[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public sealed class HttpMapOptionsAttribute : HttpMapAttribute
{
    public HttpMapOptionsAttribute(params string[] routes) : base(HttpMethod.Options, routes) { }
}


