# -*- coding: utf-8 -*-
"""
XJB Generate Images Module (/doodle)
Created on Sun Sep  1 16:03:16 2019

@author: user
"""
import os
import asyncio
import uuid

import tg_connection


gen_path = "D:/AndroidProjects/ScarletKindom/flandre-generator/wgan/sample.png"
inp_base = "D:/AndroidProjects/ScarletKindom/flandre-generator/wgan/"
sketchr_query = set()


def get_pf_path():
    pf = str(uuid.uuid4())
    return inp_base + "sketch-input-" + pf, inp_base + "sketch-output-" + pf + ".txt"


async def command_doodle(session, chat_id):
    while not os.path.exists(gen_path):
        await asyncio.sleep(0.6)
    with open(gen_path, "rb") as fi:
        png_bytes = fi.read(-1)
    os.unlink(gen_path)
    await tg_connection.send_photo_by_contents(session, chat_id, png_bytes)


async def command_sketchr(session, chat_id, from_user):
    sketchr_query.add(from_user)
    await tg_connection.send_msg(session, chat_id, "Send me a sketch!")


async def command_sketchr_clbk(session, chat_id, frid, msg):
    if frid in sketchr_query:
        sketchr_query.remove(frid)
    else:
        return
    if "photo" in msg:
        fid = msg["photo"][0]["file_id"]
        b = await tg_connection.get_file_contents(session, fid)
        fi, fo = get_pf_path()
        with open(fi, "wb") as writer:
            writer.write(b)
        while os.path.exists(fi):
            await asyncio.sleep(0.6)
        with open(fo, "rt") as reader:
            txt = reader.read(-1)
            await tg_connection.send_msg(session, chat_id, txt)
        os.unlink(fo)
