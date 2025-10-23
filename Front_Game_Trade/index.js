
document.addEventListener("DOMContentLoaded", () => {
  const btnTienda = document.getElementById("btnTienda");
  const btnLogin = document.getElementById("btnLogin");

  btnTienda.addEventListener("click", () => {
    window.location.href = "tienda.html";
  });

  btnLogin.addEventListener("click", () => {
    window.location.href = "login.html";
  });
});
