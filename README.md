# PasswordManager
<b> Password manager written in Python and C# </b>

The project has a backend written in Python for passwords stored as encrypted strings.  
Frontend of the application is written in C# using WPF and uses Linq for sorting and filtering results.  
Encryption keys (the project uses RSA and AES) are stored locally.

## Launching the project
- Clone the repository or download the project as a .zip file and extract.  
- Backend (if using Pycharm) - add .venv and activate it  
- Frontend (solution in Visual Studio) should build without any issues  
  
Backend, when launched, creates a database in the <b>/instance</b> folder called <b>passwords.db</b>.  
Frontend, when launched, creates a new folder called <b>/keys</b> in which the encryption keys are stored.