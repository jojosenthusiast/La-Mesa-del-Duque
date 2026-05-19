(function () {
    function closeToast(toast) {
        if (!toast) {
            return;
        }

        toast.classList.remove("show");
        setTimeout(function () { toast.remove(); }, 150);
    }

    function getToastZone() {
        return document.getElementById("lmd-toast-zone") || document.querySelector(".lmd-toast-zone");
    }

    window.lmdToast = function (message, type) {
        var toastZone = getToastZone();
        if (!toastZone) {
            return;
        }

        var css = "alert alert-dismissible fade show py-2 mb-2 ";
        switch (type) {
            case "success": css += "alert-success"; break;
            case "info": css += "alert-info"; break;
            case "warning": css += "alert-warning"; break;
            default: css += "alert-danger"; break;
        }

        var toast = document.createElement("div");
        toast.className = css;
        toast.setAttribute("data-lmd-toast", "true");
        toast.innerHTML = message + '<button type="button" class="btn-close" aria-label="Cerrar"></button>';
        toastZone.prepend(toast);
        setTimeout(function () { closeToast(toast); }, 4000);
    };

    window.lmdConfirm = function (message) {
        return new Promise(function (resolve) {
            var overlay = document.createElement("div");
            overlay.className = "lmd-modal-overlay";

            var modal = document.createElement("div");
            modal.className = "lmd-modal";
            modal.innerHTML = '<p>' + message + '</p>' +
                '<div class="lmd-modal-actions">' +
                '  <button type="button" class="btn btn-sm lmd-action-neutral" data-lmd-cancel>Cancelar</button>' +
                '  <button type="button" class="btn btn-sm lmd-action-danger" data-lmd-confirm>Confirmar</button>' +
                '</div>';

            overlay.appendChild(modal);
            document.body.appendChild(overlay);

            function cleanup(result) {
                overlay.remove();
                resolve(result);
            }

            modal.querySelector("[data-lmd-cancel]").addEventListener("click", function () { cleanup(false); });
            modal.querySelector("[data-lmd-confirm]").addEventListener("click", function () { cleanup(true); });
        });
    };

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
        form.addEventListener("submit", async function (event) {
            event.preventDefault();
            var message = form.getAttribute("data-confirm-message") || "¿Confirmás esta acción?";
            var ok = await window.lmdConfirm(message);
            if (ok) {
                form.submit();
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
            var text = "Pedido actualizado.";
            if (payload && payload.tipo) {
                text = payload.tipo + (payload.estado ? " · " + payload.estado : "");
            }

            window.lmdToast(text, "info");
        });

        connection.start().catch(function () {
            // noop: reconnection strategy is automatic
        });
    }
})();
