﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Lampac.Engine;
using Lampac.Engine.CORE;
using Lampac.Models.SISI;
using Microsoft.Extensions.Caching.Memory;
using System;
using Shared.Engine.SISI;

namespace Lampac.Controllers.Porntrex
{
    public class ListController : BaseController
    {
        [HttpGet]
        [Route("ptx")]
        async public Task<JsonResult> Index(string search, string sort, int pg = 1)
        {
            if (!AppInit.conf.Porntrex.enable)
                return OnError("disable");

            string memKey = $"ptx:{search}:{sort}:{pg}";
            if (!memoryCache.TryGetValue(memKey, out List<PlaylistItem> playlists))
            {
                string html = await PorntrexTo.InvokeHtml(AppInit.conf.Porntrex.host, search, sort, pg, url => HttpClient.Get(url, timeoutSeconds: 10, useproxy: AppInit.conf.Porntrex.useproxy));
                if (html == null)
                    return OnError("html");

                playlists = PorntrexTo.Playlist($"{host}/ptx/vidosik", html, pl =>
                {
                    pl.picture = HostImgProxy(0, AppInit.conf.sisi.heightPicture, pl.picture);
                    return pl;
                });

                if (playlists.Count == 0)
                    return OnError("playlists");

                memoryCache.Set(memKey, playlists, DateTime.Now.AddMinutes(AppInit.conf.multiaccess ? 10 : 2));
            }

            return new JsonResult(new
            {
                menu = PorntrexTo.Menu(host, sort),
                list = playlists
            });
        }
    }
}
