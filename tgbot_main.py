import aiohttp
import asyncio

import tg_connection
from mod_math_eval import command_calc
from mod_flan_gallery import command_flandre
from mod_flan_doodle import command_doodle, command_sketchr
from mod_flan_doodle import command_sketchr_clbk
from mod_repeater import process_repeat


def submit_coro(coro):
    asyncio.run_coroutine_threadsafe(coro, asyncio.get_event_loop())


async def main():
    next_update = 0
    async with aiohttp.ClientSession() as session:
        print("Initialize Complete.")
        while True:
            await asyncio.sleep(0.5)
            try:
                resp = await tg_connection.get_updates(session, next_update)
            except Exception as exc:
                tg_connection.try_print('Error Receiving:', repr(exc))
                continue
            for upd in resp['result']:
                next_update = max(next_update, int(upd['update_id']) + 1)
                tg_connection.try_print('Received:', upd)
                if 'message' in upd:
                    chat = upd['message']
                    cid = chat['chat']['id']
                    frid = chat['from']['id']
                    submit_coro(command_sketchr_clbk(session, cid, frid, chat))
                    if 'text' in chat:
                        comm = chat['text']
                        if not comm.startswith('/'):
                            submit_coro(process_repeat(session, cid, comm))
                        if comm.startswith('/flandre'):
                            submit_coro(command_flandre(session, cid))
                        if comm.startswith('/doodle'):
                            submit_coro(command_doodle(session, cid))
                        if comm.startswith('/sketchr'):
                            submit_coro(command_sketchr(session, cid, frid))
                        if comm.startswith('/calc'):
                            submit_coro(command_calc(session, cid, comm[5:]))


loop = asyncio.get_event_loop()
loop.run_until_complete(main())
