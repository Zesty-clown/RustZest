import secrets
import sqlite3
import os
import time

def create_connection(db_file):
    """Create a database connection to a SQLite database."""
    conn = None
    try:
        conn = sqlite3.connect(db_file)
        print(f"Connected to SQLite database: {db_file}")
    except sqlite3.Error as e:
        print(f"Error connecting to database: {e}")
    return conn

def create_table(conn):
    """Create a table to store tokens."""
    try:
        sql_create_tokens_table = """CREATE TABLE IF NOT EXISTS tokens (
                                        id INTEGER PRIMARY KEY,
                                        token TEXT NOT NULL,
                                        expiration INTEGER NOT NULL
                                    );"""
        c = conn.cursor()
        c.execute(sql_create_tokens_table)
    except sqlite3.Error as e:
        print(f"Error creating table: {e}")

def save_token_to_db(conn, token, expiration):
    """Save a token to the database with an expiration time."""
    try:
        sql_insert_token = """INSERT INTO tokens(token, expiration) VALUES(?, ?);"""
        c = conn.cursor()
        c.execute(sql_insert_token, (token, expiration))
        conn.commit()
        print("Token saved to database.")
    except sqlite3.Error as e:
        print(f"Error saving token to database: {e}")

def generate_token():
    return secrets.token_hex(16)  # Generate a secure token

def main():
    database = os.path.join(os.path.join(os.environ['USERPROFILE']), 'Desktop', 'auth_tokens.db')
    
    # Create a database connection
    conn = create_connection(database)
    
    # Create table
    if conn is not None:
        create_table(conn)
        
        # Generate and save 10 second token
        token_10_sec = generate_token()
        expiration_10_sec = int(time.time()) + 10
        print(f"Generated 10-second Token: {token_10_sec}")
        save_token_to_db(conn, token_10_sec, expiration_10_sec)
        
        # Generate and save 2 minute token
        token_2_min = generate_token()
        expiration_2_min = int(time.time()) + 120
        print(f"Generated 2-minute Token: {token_2_min}")
        save_token_to_db(conn, token_2_min, expiration_2_min)
        
        # Close the connection
        conn.close()
    else:
        print("Error! Cannot create the database connection.")

if __name__ == "__main__":
    main()
