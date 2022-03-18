(function(e) {
    function t(t) {
        for (var r, o, u = t[0], c = t[1], s = t[2], l = 0, d = []; l < u.length; l++) o = u[l], Object.prototype.hasOwnProperty.call(a, o) && a[o] && d.push(a[o][0]), a[o] = 0;
        for (r in c) Object.prototype.hasOwnProperty.call(c, r) && (e[r] = c[r]);
        f && f(t);
        while (d.length) d.shift()();
        return i.push.apply(i, s || []), n()
    }

    function n() {
        for (var e, t = 0; t < i.length; t++) {
            for (var n = i[t], r = !0, o = 1; o < n.length; o++) {
                var u = n[o];
                0 !== a[u] && (r = !1)
            }
            r && (i.splice(t--, 1), e = c(c.s = n[0]))
        }
        return e
    }
    var r = {},
        o = {
            app: 0
        },
        a = {
            app: 0
        },
        i = [];

    function u(e) {
        return c.p + "js/" + ({} [e] || e) + "." + {
            "chunk-343b4158": "eec10086",
            "chunk-7d64fef4": "f0bd1e54"
        } [e] + ".js"
    }

    function c(t) {
        if (r[t]) return r[t].exports;
        var n = r[t] = {
            i: t,
            l: !1,
            exports: {}
        };
        return e[t].call(n.exports, n, n.exports, c), n.l = !0, n.exports
    }
    c.e = function(e) {
        var t = [],
            n = {
                "chunk-343b4158": 1,
                "chunk-7d64fef4": 1
            };
        o[e] ? t.push(o[e]) : 0 !== o[e] && n[e] && t.push(o[e] = new Promise((function(t, n) {
            for (var r = "css/" + ({} [e] || e) + "." + {
                    "chunk-343b4158": "c4492452",
                    "chunk-7d64fef4": "b20b7f0e"
                } [e] + ".css", a = c.p + r, i = document.getElementsByTagName("link"), u = 0; u < i.length; u++) {
                var s = i[u],
                    l = s.getAttribute("data-href") || s.getAttribute("href");
                if ("stylesheet" === s.rel && (l === r || l === a)) return t()
            }
            var d = document.getElementsByTagName("style");
            for (u = 0; u < d.length; u++) {
                s = d[u], l = s.getAttribute("data-href");
                if (l === r || l === a) return t()
            }
            var f = document.createElement("link");
            f.rel = "stylesheet", f.type = "text/css", f.onload = t, f.onerror = function(t) {
                var r = t && t.target && t.target.src || a,
                    i = new Error("Loading CSS chunk " + e + " failed.\n(" + r + ")");
                i.code = "CSS_CHUNK_LOAD_FAILED", i.request = r, delete o[e], f.parentNode.removeChild(f), n(i)
            }, f.href = a;
            var p = document.getElementsByTagName("head")[0];
            p.appendChild(f)
        })).then((function() {
            o[e] = 0
        })));
        var r = a[e];
        if (0 !== r)
            if (r) t.push(r[2]);
            else {
                var i = new Promise((function(t, n) {
                    r = a[e] = [t, n]
                }));
                t.push(r[2] = i);
                var s, l = document.createElement("script");
                l.charset = "utf-8", l.timeout = 120, c.nc && l.setAttribute("nonce", c.nc), l.src = u(e);
                var d = new Error;
                s = function(t) {
                    l.onerror = l.onload = null, clearTimeout(f);
                    var n = a[e];
                    if (0 !== n) {
                        if (n) {
                            var r = t && ("load" === t.type ? "missing" : t.type),
                                o = t && t.target && t.target.src;
                            d.message = "Loading chunk " + e + " failed.\n(" + r + ": " + o + ")", d.name = "ChunkLoadError", d.type = r, d.request = o, n[1](d)
                        }
                        a[e] = void 0
                    }
                };
                var f = setTimeout((function() {
                    s({
                        type: "timeout",
                        target: l
                    })
                }), 12e4);
                l.onerror = l.onload = s, document.head.appendChild(l)
            } return Promise.all(t)
    }, c.m = e, c.c = r, c.d = function(e, t, n) {
        c.o(e, t) || Object.defineProperty(e, t, {
            enumerable: !0,
            get: n
        })
    }, c.r = function(e) {
        "undefined" !== typeof Symbol && Symbol.toStringTag && Object.defineProperty(e, Symbol.toStringTag, {
            value: "Module"
        }), Object.defineProperty(e, "__esModule", {
            value: !0
        })
    }, c.t = function(e, t) {
        if (1 & t && (e = c(e)), 8 & t) return e;
        if (4 & t && "object" === typeof e && e && e.__esModule) return e;
        var n = Object.create(null);
        if (c.r(n), Object.defineProperty(n, "default", {
                enumerable: !0,
                value: e
            }), 2 & t && "string" != typeof e)
            for (var r in e) c.d(n, r, function(t) {
                return e[t]
            }.bind(null, r));
        return n
    }, c.n = function(e) {
        var t = e && e.__esModule ? function() {
            return e["default"]
        } : function() {
            return e
        };
        return c.d(t, "a", t), t
    }, c.o = function(e, t) {
        return Object.prototype.hasOwnProperty.call(e, t)
    }, c.p = "", c.oe = function(e) {
        throw console.error(e), e
    };
    var s = window["webpackJsonp"] = window["webpackJsonp"] || [],
        l = s.push.bind(s);
    s.push = t, s = s.slice();
    for (var d = 0; d < s.length; d++) t(s[d]);
    var f = l;
    i.push([0, "chunk-vendors"]), n()
})({
    0: function(e, t, n) {
        e.exports = n("56d7")
    },
    "56d7": function(e, t, n) {
        "use strict";
        n.r(t);
        n("e260"), n("e6cf"), n("cca6"), n("a79d");
        var r = n("2b0e"),
            o = function() {
                var e = this,
                    t = e.$createElement,
                    n = e._self._c || t;
                return n("div", {
                    attrs: {
                        id: "app"
                    }
                }, [n("div", {
                    staticClass: "shop"
                }, [e._m(0), n("div", {
                    staticClass: "content"
                }, [n("div", {
                    staticClass: "sidebar"
                }, [e._l(e.buttons, (function(t, r) {
                    return n("SidebarButton", {
                        key: r,
                        attrs: {
                            active: e.currentTab === t.id,
                            title: t.name
                        },
                        nativeOn: {
                            click: function(n) {
                                e.currentTab = t.id
                            }
                        }
                    })
                })), n("div", {
                    staticClass: "exit"
                }, [n("button", {
                    on: {
                        click: e.exit
                    }
                }, [e._v("Выход")])])], 2), n("div", {
                    staticClass: "market"
                }, e._l(e.market_items[e.currentTab], (function(e, t) {
                    return n("MarketItem", {
                        key: t,
                        attrs: {
                            item: e
                        }
                    })
                })), 1)])])])
            },
            a = [function() {
                var e = this,
                    t = e.$createElement,
                    n = e._self._c || t;
                return n("div", {
                    staticClass: "header"
                }, [n("span", [e._v("МАГАЗИН")])])
            }],
            i = (n("d3b7"), {
                name: "App",
                components: {
                    MarketItem: function() {
                        return n.e("chunk-7d64fef4").then(n.bind(null, "c1aa"))
                    },
                    SidebarButton: function() {
                        return n.e("chunk-343b4158").then(n.bind(null, "d91e"))
                    }
                },
                data: function() {
                    return {
                        buttons: [{
                            id: 0,
                            name: "Товар"
                        }],
                        market_items: [
							[{}],
							[{}],
							[{}],
                        ],
                        currentTab: 0
                    }
                },
                created: function() {
                    document.addEventListener("keyup", this.myMethod)
                },
                methods: {
                    exit: function() {
                        window.mp.trigger("Close_new_shop")
                    },
                    myMethod: function(e) {
                        27 == e.keyCode && this.exit()
                    }
                }
            }),
            u = i,
            c = (n("5c0b"), n("2877")),
            s = Object(c["a"])(u, o, a, !1, null, null, null),
            l = s.exports;
        r["a"].config.productionTip = !1, window.app_shop = new r["a"]({
            render: function(e) {
                return e(l)
            }
        }).$mount("#app")
    },
    "5c0b": function(e, t, n) {
        "use strict";
        n("9c0c")
    },
    "9c0c": function(e, t, n) {}
});