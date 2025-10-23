document.addEventListener("DOMContentLoaded", () => {
  console.log("JS cargado correctamente");

  // --- Overlay SUBIR ---
  const botonAbrir = document.querySelector(".btn-upload");
  const overlay = document.getElementById("overlay");
  const cerrar = document.getElementById("cerrarModal");

  if (botonAbrir && overlay) {
    botonAbrir.addEventListener("click", () => {
      overlay.style.display = "flex";
    });
  }

  if (cerrar) {
    cerrar.addEventListener("click", () => {
      overlay.style.display = "none";
    });
  }

  // --- Overlay RESEÑA ---
  const btnsResena = document.querySelectorAll(".btn-resena");
  const overlayResena = document.getElementById("overlay-resena");
  const cerrarResena = document.getElementById("cerrarResena");

  btnsResena.forEach((btn) => {
    btn.addEventListener("click", () => {
      console.log("🎮 Botón clickeado:", btn.innerText);
      overlayResena.style.display = "flex";
    });
  });

  if (cerrarResena) {
    cerrarResena.addEventListener("click", () => {
      overlayResena.style.display = "none";
    });
  }

  // --- Sistema de estrellas ---
  const estrellas = document.querySelectorAll(".estrella");
  estrellas.forEach((estrella) => {
    estrella.addEventListener("click", () => {
      const valor = estrella.getAttribute("data-valor");
      estrellas.forEach((e) => e.classList.remove("activa"));
      for (let i = 0; i < valor; i++) {
        estrellas[i].classList.add("activa");
      }
    });
  });

  // --- Cerrar sesión ---
  const btnLogout = document.querySelector(".btn-logout");
  if (btnLogout) {
    btnLogout.addEventListener("click", () => {
      window.location.href = "login.html";
    });
  }

  // --- Acciones adicionales ---
  const botonesDescargar = document.querySelectorAll(".btn-secondary");
  botonesDescargar.forEach((boton) => {
    boton.addEventListener("click", () => {
      alert("Descarga iniciada... 💾");
    });
  });
});
