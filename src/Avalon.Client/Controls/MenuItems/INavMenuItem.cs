/*
 * Avalon Mud Client
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2023 All rights reserved.
 * @license           : MIT
 */

using MahApps.Metro.IconPacks;

namespace Avalon.Controls
{
    public interface INavMenuItem
    { 
        PackIconMaterialKind Icon { get; set; }

        string Title { get; set; }

        public string Argument { get; set; }

        Task ExecuteAsync();

        public INavMenuItem Reference { get; }

    }
}