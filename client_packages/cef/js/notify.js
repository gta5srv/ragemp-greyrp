function notify(type, layout, message, time) {
    var types = ['alert', 'error', 'success', 'information', 'warning'];
    var layouts = ['top', 'topLeft', 'topCenter', 'topRight', 'center', 'centerLeft', 'centerRight', 'bottom', 'bottomLeft', 'bottomCenter', 'bottomRight'];
    var icons = ['<i class=""><img src="images/UI/notification/error.png" width="25px" style="width: 30px;  font-family: TT Norms;font-style: bold;font-weight: bold;font-size: 15px;margin-top: -15px;"></i>', '<i class=""><img src="images/UI/notification/error.png" width="25px" style="width: 30px;  font-family: TT Norms;font-style: bold;font-weight: bold;font-size: 15px;margin-top: -15px;"></i>', '<i class=""><img src="images/UI/notification/success.png" width="25px" style="width: 30px;  font-family: TT Norms;font-style: bold;font-weight: bold;font-size: 15px;margin-top: -15px;"></i>', '<i class=""><img src="images/UI/notification/info.png" width="25px" style="width: 30px;  font-family: TT Norms;font-style: bold;font-weight: bold;font-size: 15px;margin-top: -15px;"></i>', '<i class=""> <img src="images/UI/notification/warning.png" width="25px"  style="width: 30px;  font-family: TT Norms;font-style: bold;font-weight: bold;font-size: 15px;margin-top: -15px;"></i>']
    message = '<div class="text">'+icons[type]+message+'</div>';
    new Noty({
        type: types[type],
        layout: layouts[layout],
        theme: 'fivestar',
        text: message,
        timeout: time,
        progressBar: true,
        animation: {
            open: 'noty_effects_open',
            close: 'noty_effects_close'
        }
    }).show();
}

