import pandas as pd

from sklearn.model_selection import train_test_split

import tensorflow as tf
from keras.models import Sequential
from keras.layers import TextVectorization, Embedding, LSTM, Dense, Input, Dropout, Bidirectional, Conv1D, GlobalMaxPooling1D

# Load the data from the CSV file
df = pd.read_csv('archive\movies_metadata.csv')
df.dropna(inplace=True)
df.drop_duplicates()
# Split the data into training and testing sets
train_data, test_data, train_label, test_label = train_test_split(df.overview, df.vote_average, test_size=0.2, random_state=42)

train_label = train_label.astype(float)
test_label = test_label.astype(float)

# Create a TextVectorization layer to preprocess the text data
vectorizer = TextVectorization(output_mode='int')

# Adapt the TextVectorization layer to the training data
vectorizer.adapt(df.overview.values)

# Define the model architecture
model = Sequential([
    Input(shape=(1,), dtype=tf.string),
    vectorizer,
    Embedding(input_dim=len(vectorizer.get_vocabulary()), output_dim=64, mask_zero=True),
    Bidirectional(LSTM(units=64, dropout=.2, )),
    Dropout(.2),
    Dense(units=32, activation='relu'),
    Dropout(.2),
    Dense(units=1, activation='linear')
])

# Compile the model
model.compile(loss='mean_squared_error', optimizer='adam', metrics=['mae'])

# Train the model
early_stop = tf.keras.callbacks.EarlyStopping(monitor='val_loss', patience=10, mode='min', restore_best_weights=True)

history = model.fit(train_data, train_label, epochs=100, validation_data=(test_data, test_label), batch_size=16, callbacks=[early_stop])

# Evaluate the model on the testing set
loss, mae = model.evaluate(test_data, test_label)

# Print the mean absolute error
print("Mean Absolute Error:", mae)

model.save("movie_rater_model_0")


while True:
    print(model.predict([input("Description: ")])[0][0])
