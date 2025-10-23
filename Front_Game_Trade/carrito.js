document.addEventListener("DOMContentLoaded", () => {
  const overlay = document.getElementById("overlay-carrito");
  const cerrar = document.getElementById("cerrarCarrito");
  const btnCart = document.querySelector(".btn-cart");
  const listaCarrito = document.getElementById("lista-carrito");
  const totalPrecio = document.getElementById("total-precio");

  // Estado del carrito
  let carrito = [];

  // Abrir carrito
  if (btnCart) {
    btnCart.addEventListener("click", () => {
      overlay.style.display = "flex";
      actualizarCarrito();
    });
  }

  // Cerrar carrito
  if (cerrar) {
    cerrar.addEventListener("click", () => {
      overlay.style.display = "none";
    });
  }

  // Simulación: agregar producto desde tienda o perfil
  document.querySelectorAll(".btn-secondary, .btn-buy").forEach(boton => {
    boton.addEventListener("click", (e) => {
      const card = e.target.closest(".juego, .product-card");
      const nombre = card.querySelector("p, h3").innerText;
      const precioTexto = card.querySelector("p:has(span), p").innerText.match(/\d+/);
      const precio = precioTexto ? parseInt(precioTexto[0]) : 40;
      const img = card.querySelector("img").src;

      carrito.push({ nombre, precio, img });
      alert(`🛒 ${nombre} agregado al carrito`);
      actualizarCarrito();
    });
  });

  // Actualizar lista
  function actualizarCarrito() {
    listaCarrito.innerHTML = "";
    let total = 0;

    carrito.forEach((item) => {
      total += item.precio;
      const div = document.createElement("div");
      div.classList.add("item-carrito");
      div.innerHTML = `
        <img src="${item.img}">
        <div class="item-info">${item.nombre}</div>
        <span>$${item.precio}.000</span>
      `;
      listaCarrito.appendChild(div);
    });

    totalPrecio.textContent = `$${total}.000`;
  }
});
