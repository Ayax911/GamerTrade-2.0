La API fue creada como una herramienta genérica que permite trabajar con cualquier tabla de la base de datos sin necesidad de crear controladores o servicios específicos. Su función principal es facilitar operaciones CRUD (crear, leer, actualizar y eliminar) y ejecutar procedimientos almacenados desde una aplicación web.

La API usa un esquema en capas, donde los controladores reciben las solicitudes, los servicios manejan la lógica y los repositorios se encargan del acceso a datos. Para conectarse a la base de datos utiliza una fábrica de conexiones que permite configurar diferentes orígenes de datos según sea necesario.

La comunicación entre el cliente y la API se realiza en formato JSON y se manejan códigos HTTP para indicar los resultados de las operaciones. Además, la API implementa autenticación mediante tokens JWT para asegurar que solo usuarios autorizados puedan hacer peticiones.
