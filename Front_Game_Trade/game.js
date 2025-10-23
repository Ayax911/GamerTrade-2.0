document.addEventListener("DOMContentLoaded", () => {
  const btnComprar = document.querySelector(".buy");
  const btnIntercambiar = document.querySelector(".trade");
  const btnLogout = document.querySelector(".btn-logout");

  btnComprar.addEventListener("click", () => {
    alert("🛒 Se ha agregado al carrito de compras");
  });

  btnIntercambiar.addEventListener("click", () => {
    alert("🔁 Función de intercambio disponible próximamente.");
  });

  btnLogout.addEventListener("click", () => {
    window.location.href = "login.html";
  });
});
