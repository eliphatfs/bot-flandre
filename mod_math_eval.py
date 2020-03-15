# -*- coding: utf-8 -*-
"""
Maths and Evaluations Module (/calc)
Created on Sun Sep  1 15:22:04 2019

@author: eliphat
"""
import numpy
import scipy.integrate as iint
import sympy
import random
import tg_connection
import multiprocessing as mp
import queue


def env_func(nump_f, symp_f):
    def _func(x):
        if isinstance(x, (sympy.Symbol, sympy.Expr)):
            return symp_f(x)
        return nump_f(x)
    return _func


def eval_env():
    return {
        'exp': env_func(numpy.exp, sympy.exp),
        'ln': env_func(numpy.log, sympy.log),
        'log': env_func(numpy.log, sympy.log),
        'sin': env_func(numpy.sin, sympy.sin),
        'cos': env_func(numpy.cos, sympy.cos),
        'pi': numpy.pi,
        'e': numpy.exp(1.0),
        'tan': env_func(numpy.tan, sympy.tan),
        'arctan': env_func(numpy.arctan, sympy.atan),
        'arcsin': env_func(numpy.arcsin, sympy.asin),
        'arccos': env_func(numpy.arccos, sympy.acos),
        'diff': lambda function, var: function.diff(var),
        'integrate': sympy.integrate,
        'defint': lambda f, low, high: iint.quad(f, low, high)[0],
        'sqrt': env_func(numpy.sqrt, sympy.sqrt),
        'factor_expr': sympy.factor,
        'simplify_expr': lambda expr: sympy.cancel(sympy.simplify(expr)),
        'expand_expr': sympy.expand,
        'oo': sympy.oo,
        'inf': numpy.inf,
        'limit': sympy.limit,
        'r': random,
        'exit': lambda: 0,
        'Symbol': sympy.Symbol
    }


def _target(q, cst):
    try:
        q.put(str(eval(cst, eval_env()))[:200])
    except Exception as exc:
        q.put("Error Calculating: " + str(exc.args))


def internal_calc(cal_str):
    cal_str = cal_str.lower()
    prev = []
    for ch in cal_str:
        if ch in ('\r', '\n'):
            continue
        if len(prev) > 0 and prev[-1] in '@`':
            prev[-1] = "Symbol('%s')" % ch
            continue
        prev.append(ch)
    cal_str = ''.join(prev)
    if 'eval' in cal_str or 'import' in cal_str or 'getattr' in cal_str:
        return "Error Calculating"
    elif 'open' in cal_str or 'del' in cal_str or 'load' in cal_str:
        return "Error Calculating"
    elif 'exec' in cal_str or 'globals' in cal_str or 'locals' in cal_str:
        return "Error Calculating"
    elif 'while' in cal_str or 'for' in cal_str:
        return "Error Calculating"
    elif ';' in cal_str:
        return "Error Calculating"
    else:
        try:
            q = mp.Queue()
            p = mp.Process(target=_target, args=(q, cal_str))
            p.start()
            try:
                return q.get(True, 4.0)
            except queue.Empty:
                p.terminate()
                return "Time Limit Exceed"
        except Exception as exc:
            return "Error Calculating: " + str(exc.args)


async def command_calc(session, chat_id, expr):
    res = internal_calc(expr)
    await tg_connection.send_msg(session, chat_id, res)
