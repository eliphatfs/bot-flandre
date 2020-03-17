# -*- coding: utf-8 -*-
"""
Created on Wed Feb 26 15:31:21 2020

@author: eliphat
"""
import asyncio
import aiohttp
import mod_math_eval
import mod_flan_gallery
import mod_midi_collection
import mod_music
import json
import random
import base64
# import qqbot_ipc


async def send_msg(ws, **kwargs):
    await ws.send_str(json.dumps({
        "action": "send_msg",
        "params": kwargs
    }))


def compose_image_msg(local_path):
    with open(local_path, "rb") as fp:
        contents = fp.read()
        b64 = base64.b64encode(contents).decode()
        return "[CQ:image,file=base64://%s]" % b64


def compose_voice_msg(local_path):
    with open(local_path, "rb") as fp:
        contents = fp.read()
        b64 = base64.b64encode(contents).decode()
        return "[CQ:record,file=base64://%s]" % b64


async def handler_msg(message):
    if message.startswith("#calc"):
        cmd_arg = message[5:]
        return mod_math_eval.internal_calc(cmd_arg)
    elif message.startswith("#flandre"):
        path = mod_flan_gallery.internal_local_getpic()
        return compose_image_msg(path)
    elif message.startswith("#mcollection"):
        cmd_arg = message[12:].strip()
        mus = mod_midi_collection.do_search(cmd_arg)
        if mus is None:
            return None
        return compose_voice_msg(mus)
    elif message.startswith("#note"):
        cmd_arg = message[5:]
        mus = mod_music.do_music(cmd_arg)
        if mus is None:
            return None
        t, fi = mus
        if t == 's':
            return fi
        return compose_voice_msg(fi)
    return None


async def handle(ws, mdata):
    if mdata.get("post_type") != "message":
        return
    message = mdata["message"]
    print("Received:", message)
    await asyncio.sleep(random.random() + 0.2)
    res = await handler_msg(message)
    # print("Response:", res)
    if res is None:
        return
    if mdata.get("message_type") == "group":
        await send_msg(ws, group_id=mdata["group_id"], message=res)
    elif mdata.get("message_type") == "private":
        await send_msg(ws, user_id=mdata["user_id"], message=res)


async def main_loop():
    async with aiohttp.ClientSession() as session:
        async with session.ws_connect("http://127.0.0.1:6700/") as ws:
            print("WebSocket Connected!")
            async for msg in ws:
                if msg.type != aiohttp.WSMsgType.TEXT:
                    continue
                mdata = json.loads(msg.data)
                asyncio.run_coroutine_threadsafe(
                    handle(ws, mdata),
                    asyncio.get_event_loop()
                )
                await asyncio.sleep(1.0)


if __name__ == "__main__":
    asyncio.get_event_loop().run_until_complete(main_loop())
