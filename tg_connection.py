# -*- coding: utf-8 -*-
"""
Communication with TG
Created on Sun Sep  1 15:26:35 2019

@author: eliphat
"""
import json
import io

import tgconfig


token = tgconfig.token

base = "https://api.telegram.org/bot" + token

send_text_url = '{}/sendMessage'.format(base)
send_photo_url = '{}/sendPhoto'.format(base)
answer_query_url = '{}/answerInlineQuery'.format(base)
get_file_url = '{}/getFile'.format(base)
get_upd_url = '{}/getUpdates'.format(base)
download_url_fmt = "https://api.telegram.org/file/bot" + token + "/%s"


def try_print(*args, **kwargs):
    try:
        print(*args, **kwargs)
    except Exception:
        pass


async def send_msg(session, chat_id, text):
    data = {'text': text, 'chat_id': chat_id}
    async with session.get(send_text_url,
                           params=data,
                           proxy='http://127.0.0.1:1080') as response:
        try_print("Sent:", await response.json())


async def send_photo_by_url(session, chat_id, url):
    data = {'photo': url, 'chat_id': chat_id}
    async with session.get(send_photo_url,
                           params=data,
                           proxy='http://127.0.0.1:1080') as response:
        try_print("Sent:", await response.json())


async def send_photo_by_contents(session, chat_id, contents):
    pd = {'chat_id': str(chat_id), 'photo': io.BytesIO(contents)}
    async with session.post(send_photo_url,
                            data=pd,
                            proxy='http://127.0.0.1:1080') as response:
        try_print("Sent:", await response.json())


async def get_updates(session, next_update):
    async with session.get(get_upd_url,
                           params={'offset': next_update},
                           proxy='http://127.0.0.1:1080') as response:
        return await response.json()


async def respond_query(session, query_id, res):
    data = {'results': json.dumps(res), 'inline_query_id': query_id}
    async with session.get(answer_query_url,
                           params=data,
                           proxy='http://127.0.0.1:1080') as response:
        try_print("Sent:", await response.json())


async def get_file_contents(session, file_id):
    data = {'file_id': file_id}
    async with session.get(get_file_url,
                           params=data,
                           proxy='http://127.0.0.1:1080') as response:
        respath = await response.json()
        ap_path = respath["result"]["file_path"]
        furl = download_url_fmt % ap_path
        async with session.get(furl,
                               proxy='http://127.0.0.1:1080') as lnk:
            return await lnk.read()
