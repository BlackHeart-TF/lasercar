using Microsoft.Maui.Graphics;

namespace lasercar
{ 
    public class JoystickDrawable : IDrawable
    {
        private readonly float joystickRadius = 100;
        private readonly float thumbRadius = 30;
        public Point ThumbPosition { get; set; }
        public Point Center { get; private set; }

        public JoystickDrawable()
        {
            Center = new Point(joystickRadius, joystickRadius);
            ThumbPosition = Center;
        }

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            canvas.FillColor = Colors.Gray;
            canvas.FillCircle((float)Center.X, (float)Center.Y, joystickRadius);

            canvas.FillColor = Colors.DarkGray;
            canvas.FillCircle((float)ThumbPosition.X, (float)ThumbPosition.Y, thumbRadius);
        }

        public void UpdateThumbPosition(Point newThumbPosition)
        {
            // Calculate direction as a vector (represented here by a Size object)
            Size direction = newThumbPosition - Center;

            // Calculate the length of the vector
            double length = Math.Sqrt(direction.Width * direction.Width + direction.Height * direction.Height);

            // Check if the length of the direction vector is within the joystick's radius
            if (length <= joystickRadius)
            {
                ThumbPosition = newThumbPosition;
            }
            else
            {
                // Normalize the direction vector and scale it by the joystick's radius
                double normFactor = joystickRadius / length;
                Point normalizedDirection = new Point(direction.Width * normFactor, direction.Height * normFactor);
                ThumbPosition = Center + normalizedDirection;
            }
        }

    }


}
