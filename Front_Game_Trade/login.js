document.querySelector(".login-box").addEventListener("submit", (e) => {
  e.preventDefault();
  const email = document.getElementById("email").value;
  const password = document.getElementById("password").value;

  if (email === "Administrador@gmail.com" && password === "1234") {
    alert("Inicio de sesión exitoso 🎉");
  } else {
    alert("Correo o contraseña incorrectos");
  }
});
