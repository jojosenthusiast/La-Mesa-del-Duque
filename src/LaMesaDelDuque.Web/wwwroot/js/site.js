(function () {
    function closeToast(toast) {
        if (!toast) {
            return;
        }

        toast.classList.remove("show");
        setTimeout(function () { toast.remove(); }, 150);
    }

    document.addEventListener("click", function (event) {
        var closeButton = event.target.closest("[data-lmd-toast='true'] .btn-close");
        if (!closeButton) {
            return;
        }

        event.preventDefault();
        closeToast(closeButton.closest("[data-lmd-toast='true']"));
    });

    document.querySelectorAll(".lmd-auto-submit-on-change input[type='number']").forEach(function (input) {
        input.addEventListener("change", function () {
            var form = input.closest("form");
            if (form) {
                form.submit();
            }
        });
    });

    document.querySelectorAll(".lmd-confirm-destructive").forEach(function (form) {
        form.addEventListener("submit", function (event) {
            var message = form.getAttribute("data-confirm-message") || "¿Confirmás esta acción?";
            if (!window.confirm(message)) {
                event.preventDefault();
            }
        });
    });

    document.addEventListener("click", function (event) {
        var editButton = event.target.closest("[data-lmd-edit-product]");
        if (!editButton) {
            return;
        }

        var setValue = function (id, value) {
            var input = document.getElementById(id);
            if (input) {
                input.value = value || "";
            }
        };

        setValue("Vm_Form_Id", editButton.getAttribute("data-producto-id"));
        setValue("Vm_Form_Nombre", editButton.getAttribute("data-producto-nombre"));
        setValue("Vm_Form_Precio", editButton.getAttribute("data-producto-precio"));
        setValue("Vm_Form_CategoriaId", editButton.getAttribute("data-producto-categoria-id"));
        setValue("Vm_Form_Descripcion", editButton.getAttribute("data-producto-descripcion"));
        setValue("Vm_Form_ImagenUrl", editButton.getAttribute("data-producto-imagen-url"));
        setValue("Vm_Form_TiempoPreparacionMin", editButton.getAttribute("data-producto-tiempo-preparacion"));

        var formContainer = document.getElementById("producto-form");
        if (formContainer) {
            formContainer.classList.add("show");
        }
    });

    if (window.signalR && window.lmdPedidosHubUrl) {
        var connection = new window.signalR.HubConnectionBuilder()
            .withUrl(window.lmdPedidosHubUrl)
            .withAutomaticReconnect()
            .build();

        connection.on("RecibirNotificacionPedido", function (payload) {
            var toastZone = document.getElementById("lmd-toast-zone") || document.querySelector(".lmd-toast-zone");
            if (!toastZone) {
                return;
            }

            var text = "Pedido actualizado.";
            if (payload && payload.tipo) {
                text = payload.tipo + (payload.estado ? " · " + payload.estado : "");
            }

            var toast = document.createElement("div");
            toast.className = "alert alert-info alert-dismissible fade show py-2 mb-2";
            toast.setAttribute("data-lmd-toast", "true");
            toast.innerHTML = text + '<button type="button" class="btn-close" aria-label="Cerrar"></button>';
            toastZone.prepend(toast);
        });

        connection.start().catch(function () {
            // noop: reconnection strategy is automatic
        });
    }
})();
