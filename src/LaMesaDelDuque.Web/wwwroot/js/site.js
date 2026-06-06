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

    function setProductFormValue(id, value) {
        var input = document.getElementById(id);
        if (input) {
            input.value = value || "";
        }
    }

    function setProductFormMode(text) {
        var title = document.getElementById("producto-form-mode");
        if (title) {
            title.textContent = text;
        }
    }

    function openProductForm() {
        var formContainer = document.getElementById("producto-form");
        if (formContainer) {
            formContainer.classList.add("show");
        }

        var nameInput = document.getElementById("Vm_Form_Nombre");
        if (nameInput) {
            nameInput.focus();
        }
    }

    function resetProductForm() {
        setProductFormMode("Nuevo producto");
        setProductFormValue("Vm_Form_Id", "");
        setProductFormValue("Vm_Form_Nombre", "");
        setProductFormValue("Vm_Form_Precio", "");
        setProductFormValue("Vm_Form_CategoriaId", "");
        setProductFormValue("Vm_Form_Descripcion", "");
        setProductFormValue("Vm_Form_ImagenUrl", "");
        setProductFormValue("Vm_Form_TiempoPreparacionMin", "");

        var fileInput = document.getElementById("Vm_Form_ImagenFile");
        if (fileInput) {
            fileInput.value = "";
        }

        var removeImageInput = document.getElementById("Vm_Form_EliminarImagen");
        if (removeImageInput) {
            removeImageInput.checked = false;
        }
    }

    document.addEventListener("click", function (event) {
        var newButton = event.target.closest("[data-lmd-new-product]");
        if (!newButton) {
            return;
        }

        event.preventDefault();
        resetProductForm();
        openProductForm();
    });

    document.addEventListener("click", function (event) {
        var editButton = event.target.closest("[data-lmd-edit-product]");
        if (!editButton) {
            return;
        }

        setProductFormMode("Editar producto");
        setProductFormValue("Vm_Form_Id", editButton.getAttribute("data-producto-id"));
        setProductFormValue("Vm_Form_Nombre", editButton.getAttribute("data-producto-nombre"));
        setProductFormValue("Vm_Form_Precio", editButton.getAttribute("data-producto-precio"));
        setProductFormValue("Vm_Form_CategoriaId", editButton.getAttribute("data-producto-categoria-id"));
        setProductFormValue("Vm_Form_Descripcion", editButton.getAttribute("data-producto-descripcion"));
        setProductFormValue("Vm_Form_ImagenUrl", editButton.getAttribute("data-producto-imagen-url"));
        setProductFormValue("Vm_Form_TiempoPreparacionMin", editButton.getAttribute("data-producto-tiempo-preparacion"));
        openProductForm();
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
