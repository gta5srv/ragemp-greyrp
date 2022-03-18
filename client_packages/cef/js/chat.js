const chatmsgs = document.getElementById('chat_messages');
const chatvar = $("#chat");

let chat =
{
    size: 0,
    container: null,
    input: null,
    enabled: false,
    active: true,
    timer: null,
	alpha: 1, // Включена ли прозрачность
    chatsize: 0, // Step размера окна чата
    fontstep: 0, // Step размера шрифта в чате
    prevmsg: ["", "", "", "", "", "", "", "", "", ""],
    steplist: 0, // В какую ячейку истории записать следующее отправленное сообщение
    backlist: 0, // Какую ячейку сейчас просматриваем из сохраненных в истории
    timestamp: 1,
    stime: "00:00" // ServerHour:ServerMinute
};


function enableChatInput(enable) {
    if (chat.active == false && enable == true) return;
    if (enable != (chat.input != null)) {
        mp.invoke("focus", enable);
        if (enable) {
            chatvar.css("opacity", 1);
            chat.input = chatvar.append('<div class="qwe"><input type="text" id="chat_msg" placeholder="Начните писать сообщение..."><button class="img"></div>').children(":last");
            chat.input.children("input").focus();
            mp.trigger("changeChatState", true);
			
        } else {
            chat.input.fadeOut('fast', function () {
                chat.input.remove();
                chat.input = null;
                mp.trigger("changeChatState", false);
            });
        }
    }
}

var chatAPI =
{
    push: (text) => {
        chat.size++;

        if (chat.size >= 50) chat.container.children(":first").remove();
        if (chat.timestamp == 0) chat.container.append("<li>" + text + "</li>");
        else chat.container.append("<li>[" + chat.stime + "] " + text + "</li>");
        chat.container.scrollTop(9999);
    },

    clear: () => {
        chat.container.html("");
    },

    activate: (toggle) => {
        if (toggle == false && chat.input != null) enableChatInput(false);
        chat.active = toggle;
    },

    show: (toggle) => {
        if (toggle) chatvar.show();
        else chatvar.hide();
        chat.active = toggle;
    }
};

let api = {"chat:push": chatAPI.push, "chat:clear": chatAPI.clear, "chat:activate": chatAPI.activate, "chat:show": chatAPI.show}; 

for(let fn in api)
{
    mp.events.add(fn, api[fn]);
}

function hide() {
	if(chat.alpha == 1) {
		if(chat.timer != null) clearTimeout(chat.timer);
		chat.timer = setTimeout(function () {
			chatvar.css("opacity", 0.65);
		}, 30000);
	}
}
function show() {
	if(chat.timer != null) {
		clearTimeout(chat.timer);
		chat.timer = null;
	}
    chatvar.css("opacity", 1);
}

function savehistory(value) {
    if (chat.steplist < 9) {
        chat.prevmsg[chat.steplist] = value;
        chat.steplist++;
    } else {
        for (let i = 0; i != 9; i++) {
            chat.prevmsg[i] = chat.prevmsg[(i + 1)];
        }
        chat.prevmsg[chat.steplist] = value;
    }
}

var lastMessage = 0;

$(document).ready(function () {
    chat.container = $("#chat ul#chat_messages");
    hide();

    $(".ui_element").show();
	
    $("body").keydown(function (event) {
        if (event.which == 84 && chat.input == null && chat.active == true) {
            enableChatInput(true);
            event.preventDefault();
            show();
            chat.backlist = chat.steplist;
        } else if (event.which == 13 && chat.input != null) {
            var value = chat.input.children("input").val();
            if (value.length > 0 && new Date().getTime() - lastMessage > 1000) {
                savehistory(value);
                lastMessage = new Date().getTime();
                if (value[0] == "/") {
                    value = value.substr(1);
					var premade = value.split(' ')[0];
                    if (value.length > 0 && value.length <= 150) {
                        if (premade.includes("timestamp")) {
                            newcfg(0, !chat.timestamp);
							mp.trigger('chatconfig', 0, chat.timestamp);
                        } else if (premade.includes("pagesize")) {
                            chat.chatsize++;
							newcfg(1,chat.chatsize);
							mp.trigger('chatconfig', 1, chat.chatsize);
                        } else if (premade.includes("fontsize")) {
                            chat.fontstep++;
							newcfg(2,chat.fontstep);
							mp.trigger('chatconfig', 2, chat.fontstep);
                        } else if (premade.includes("chatalpha")) {
                            newcfg(3, !chat.alpha);
							mp.trigger('chatconfig', 3, chat.alpha);
                        } else mp.invoke("command", value);
                    }
                } else {
                    if (value.length <= 150) mp.invoke("chatMessage", value);
                }
            }
			chat.container.scrollTop(9999);
            enableChatInput(false);
            hide();
        } else if (event.which == 27 && chat.input != null) {
            enableChatInput(false);
            hide();
        } else if (event.which == 38 && chat.input != null) { // Листание вверх
            if(chat.steplist < 9) {
				if(chat.backlist >= 1) chat.backlist--;
				chat.input.children("input").val(chat.prevmsg[chat.backlist]);
			} else {
				chat.input.children("input").val(chat.prevmsg[chat.backlist]);
				if(chat.backlist >= 1) chat.backlist--;
			}
        } else if (event.which == 40 && chat.input != null) { // Листание вниз
            if (chat.backlist < chat.steplist) chat.backlist++;
            chat.input.children("input").val(chat.prevmsg[chat.backlist]);
        }
    });
});

// Effects //
var fxel = $('#effect');
var fx = {
    set: (style) => {
        fxel.addClass(style);
    },
    reset: () => {
        fxel.removeClass();
    }
}