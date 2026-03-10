# ERP Concesionario – Sistema de Gestión Empresarial

## Descripción del Proyecto

Este proyecto consiste en el desarrollo de un ERP para un concesionario de vehículos, cuyo objetivo es centralizar la gestión de:

- Inventario de vehículos  
- Clientes  
- Ventas  
- Facturación  
- Reportes básicos  

El sistema será desarrollado en C# utilizando herramientas del ecosistema Microsoft, bajo una arquitectura en capas que permita escalabilidad, mantenimiento y buenas prácticas de ingeniería de software.

---

## Objetivo General

Diseñar e implementar un sistema ERP básico que permita optimizar los procesos administrativos y comerciales de un concesionario automotriz.

---

## Tecnologías Utilizadas

- Lenguaje: C#  
- Framework: .NET  
- Entorno de desarrollo: Visual Studio 2022  
- Base de datos: SQL Server  
- Control de versiones: Git  
- Repositorio: GitHub  
- Metodología: Scrum  

---

## Arquitectura del Proyecto

El sistema sigue una arquitectura en capas:

```
ERP-Concesionario
├── Presentacion
├── LogicaNegocio
├── AccesoDatos
└── BaseDatos
```

- Presentación: Interfaz de usuario.  
- Lógica de Negocio: Reglas y procesos del sistema.  
- Acceso a Datos: Conexión y consultas a la base de datos.  
- Base de Datos: Modelo relacional en SQL Server.  

---

## Equipo de Desarrollo

| Rol | Integrante |
|------|------------|
| Scrum Master  | Juan Pablo Rojas |
| Product Owner | David Santiago |
| Líder Técnico | Hayder |

---

## Metodología de Trabajo

El proyecto se desarrolla bajo la metodología Scrum, con planificación por Sprints.

### Sprint 0

En esta fase se realiza:

- Configuración del entorno de desarrollo.  
- Creación del repositorio.  
- Definición de arquitectura.  
- Diseño preliminar de base de datos.  
- Planificación en Jira.  
- Configuración de la estructura base del sistema.  

Fecha de finalización Sprint 0: 22 de febrero.

---

## Control de Versiones

Se utiliza la siguiente estrategia de ramas:

- `main` → versión estable  
- `develop` → rama de integración  
- `feature/*` → ramas por funcionalidad  

Cada cambio se realiza mediante Pull Request y revisión previa antes de integrarse a la rama principal.

---

## Aseguramiento de Calidad

- Pruebas unitarias básicas.  
- Validación de conexión a base de datos.  
- Revisión cruzada de código.  
- Commits descriptivos obligatorios.  

---

## Estado Actual del Proyecto

Fase inicial – Sprint 0.  
Actualmente el proyecto se encuentra en configuración estructural y definición técnica.

---

## Próximos Módulos a Implementar

- Gestión de Clientes  
- Gestión de Inventario  
- Registro de Ventas  
- Reportes básicos  
- Control de usuarios  

---

## Licencia

Proyecto académico  
Universidad Manuela Beltrán.  
Ingeniería de Software – 2026
