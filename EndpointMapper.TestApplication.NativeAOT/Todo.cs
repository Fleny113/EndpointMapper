﻿namespace EndpointMapper.TestApplication.NativeAOT;

public record Todo(int Id, string? Title, DateOnly? DueBy = null, bool IsComplete = false);