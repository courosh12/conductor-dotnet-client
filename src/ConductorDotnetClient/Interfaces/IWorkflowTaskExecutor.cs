﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ConductorDotnetClient.Interfaces
{
    public interface IWorkflowTaskExecutor
    {
        Task StartPoller();
    }
}
