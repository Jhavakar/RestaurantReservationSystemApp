# Restaurant Reservation System

A web application for managing restaurant reservations, built with C# for the backend, Angular for the frontend, and SQLite as the database.

## Table of Contents

- [Installation](#installation)
- [Usage](#usage)
- [Contributing](#contributing)
- [License](#license)

## Installation

### Backend (C#)

1. **Clone the repository**:

    ```bash
    git clone https://github.com/your-username/your-repository.git
    ```

2. **Navigate to the backend project directory**:

    ```bash
    cd your-repository/backend
    ```

3. **Restore the .NET dependencies**:

    ```bash
    dotnet restore
    ```

4. **Run the database migrations** (assuming you are using Entity Framework Core):

    ```bash
    dotnet ef database update
    ```

5. **Run the backend server**:

    ```bash
    dotnet run
    ```

### Frontend (Angular)

1. **Navigate to the frontend project directory**:

    ```bash
    cd ../frontend
    ```

2. **Install Angular dependencies**:

    ```bash
    npm install
    ```

3. **Run the Angular development server**:

    ```bash
    ng serve
    ```

### SQLite Database

1. **Ensure SQLite is installed**:
    - SQLite comes bundled with most systems. You can check by running `sqlite3` in your command line.

2. **Database setup**:
    - The database should be automatically set up with the Entity Framework migrations. If not, ensure the connection string in your C# project points to a valid SQLite database file.

## Usage

1. **Access the application**:
    - Open a web browser and navigate to `http://localhost:4200` for the Angular frontend.
    - The backend API should be running on `http://localhost:5000` by default.

2. **Using the application**:
    - Users can sign up, log in, and manage reservations.
    - Admins can view all reservations and manage users.

## Contributing

Guidelines on how to contribute to the project.

1. **Fork the repository**.
2. **Create a new branch**:

    ```bash
    git checkout -b feature/your-feature-name
    ```

3. **Make your changes**.
4. **Commit your changes**:

    ```bash
    git commit -m "Add some feature"
    ```

5. **Push to the branch**:

    ```bash
    git push origin feature/your-feature-name
    ```

6. **Open a pull request**.

## License

Specify the license under which the project is distributed.

Project Link: https://github.com/Jhavakar/RestaurantReservationSystemApp
