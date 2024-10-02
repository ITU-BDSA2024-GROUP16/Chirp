#!/bin/bash

# Check for CHIRPDBPATH environment variable
if [ -z "$CHIRPDBPATH" ]; then
    echo "Warning: CHIRPDBPATH is not set. Using default path '/tmp/chirp.db'."
    DB_PATH="/tmp/chirp.db"
else
    DB_PATH="$CHIRPDBPATH"
fi

# Create the database and initialize it
sqlite3 "$DB_PATH" < ../data/schema.sql
sqlite3 "$DB_PATH" < ../data/dump.sql

echo "Database initialized at $DB_PATH."
